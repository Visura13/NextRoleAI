import 'dart:async';
import 'dart:convert';

import 'package:http/http.dart' as http;

import '../../features/auth/models/auth_models.dart';
import '../storage/session_storage.dart';

class ApiException implements Exception {
  const ApiException(
    this.message, {
    required this.statusCode,
    this.errors = const {},
  });

  final String message;
  final int statusCode;
  final Map<String, List<String>> errors;

  @override
  String toString() => message;
}

class ApiClient {
  ApiClient({
    required String baseUrl,
    required SessionStorage sessionStorage,
    http.Client? httpClient,
  }) : _baseUrl = Uri.parse(baseUrl),
       // ignore: prefer_initializing_formals
       _sessionStorage = sessionStorage,
       _httpClient = httpClient ?? http.Client();

  final Uri _baseUrl;
  final SessionStorage _sessionStorage;
  final http.Client _httpClient;
  Future<bool>? _refreshInFlight;

  Future<Map<String, dynamic>> getJson(
    String path, {
    Map<String, String?> query = const {},
    bool authenticated = true,
  }) => _request('GET', path, query: query, authenticated: authenticated);

  Future<Map<String, dynamic>> postJson(
    String path, {
    Object? body,
    bool authenticated = true,
  }) => _request('POST', path, body: body, authenticated: authenticated);

  Future<Map<String, dynamic>> putJson(
    String path, {
    Object? body,
    bool authenticated = true,
  }) => _request('PUT', path, body: body, authenticated: authenticated);

  Future<void> postEmpty(
    String path, {
    Object? body,
    bool authenticated = true,
  }) async {
    await _request('POST', path, body: body, authenticated: authenticated);
  }

  Future<Map<String, dynamic>> _request(
    String method,
    String path, {
    Map<String, String?> query = const {},
    Object? body,
    required bool authenticated,
    bool allowRefresh = true,
  }) async {
    final response = await _send(
      method,
      path,
      query: query,
      body: body,
      authenticated: authenticated,
    );

    if (response.statusCode == 401 &&
        authenticated &&
        allowRefresh &&
        await _refresh()) {
      return _request(
        method,
        path,
        query: query,
        body: body,
        authenticated: true,
        allowRefresh: false,
      );
    }

    return _decode(response);
  }

  Future<http.Response> _send(
    String method,
    String path, {
    Map<String, String?> query = const {},
    Object? body,
    required bool authenticated,
  }) async {
    final filteredQuery = <String, String>{
      for (final entry in query.entries)
        if (entry.value != null && entry.value!.isNotEmpty)
          entry.key: entry.value!,
    };
    final relative = Uri.parse(path);
    final uri = _baseUrl.resolveUri(
      relative.replace(
        queryParameters: filteredQuery.isEmpty ? null : filteredQuery,
      ),
    );
    final request = http.Request(method, uri)
      ..headers['Accept'] = 'application/json';

    if (body != null) {
      request.headers['Content-Type'] = 'application/json';
      request.body = jsonEncode(body);
    }

    if (authenticated) {
      final session = await _sessionStorage.read();
      if (session != null) {
        request.headers['Authorization'] = 'Bearer ${session.accessToken}';
      }
    }

    final streamed = await _httpClient
        .send(request)
        .timeout(
          const Duration(seconds: 20),
          onTimeout: () => throw const ApiException(
            'The server took too long to respond.',
            statusCode: 408,
          ),
        );
    return http.Response.fromStream(streamed);
  }

  Future<bool> _refresh() {
    final existing = _refreshInFlight;
    if (existing != null) return existing;

    final refresh = _performRefresh();
    _refreshInFlight = refresh;
    return refresh.whenComplete(() => _refreshInFlight = null);
  }

  Future<bool> _performRefresh() async {
    final session = await _sessionStorage.read();
    if (session == null) return false;

    try {
      final response = await _send(
        'POST',
        '/api/auth/refresh',
        body: {'refreshToken': session.refreshToken},
        authenticated: false,
      );
      if (response.statusCode < 200 || response.statusCode >= 300) {
        await _sessionStorage.clear();
        return false;
      }
      await _sessionStorage.write(
        AuthSession.fromJson(jsonDecode(response.body) as Map<String, dynamic>),
      );
      return true;
    } on Object {
      await _sessionStorage.clear();
      return false;
    }
  }

  Map<String, dynamic> _decode(http.Response response) {
    if (response.statusCode >= 200 && response.statusCode < 300) {
      if (response.body.isEmpty) return const {};
      return jsonDecode(response.body) as Map<String, dynamic>;
    }

    Map<String, dynamic> problem = const {};
    try {
      problem = jsonDecode(response.body) as Map<String, dynamic>;
    } on Object {
      // The fallback below handles non-JSON server responses.
    }

    final rawErrors = problem['errors'];
    final errors = rawErrors is Map<String, dynamic>
        ? rawErrors.map(
            (key, value) => MapEntry(
              key,
              (value as List<dynamic>).map((item) => item.toString()).toList(),
            ),
          )
        : <String, List<String>>{};
    final firstError = errors.values
        .where((value) => value.isNotEmpty)
        .firstOrNull
        ?.first;
    throw ApiException(
      firstError ??
          problem['detail']?.toString() ??
          problem['title']?.toString() ??
          'The request could not be completed.',
      statusCode: response.statusCode,
      errors: errors,
    );
  }

  void close() => _httpClient.close();
}
