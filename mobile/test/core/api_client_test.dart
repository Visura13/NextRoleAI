import 'dart:convert';

import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:nextroleai_mobile/core/api/api_client.dart';

import '../support/fakes.dart';

void main() {
  test('adds the bearer token to authenticated requests', () async {
    final storage = MemorySessionStorage(sampleSession());
    final client = ApiClient(
      baseUrl: 'https://api.example.test',
      sessionStorage: storage,
      httpClient: MockClient((request) async {
        expect(request.headers['Authorization'], 'Bearer access-token');
        return http.Response('{"value":"ok"}', 200);
      }),
    );

    final result = await client.getJson('/api/private');

    expect(result['value'], 'ok');
  });

  test('refreshes once after a 401 and retries with the new token', () async {
    final storage = MemorySessionStorage(sampleSession(accessToken: 'old'));
    var protectedCalls = 0;
    final client = ApiClient(
      baseUrl: 'https://api.example.test',
      sessionStorage: storage,
      httpClient: MockClient((request) async {
        if (request.url.path == '/api/auth/refresh') {
          final refreshed = {
            ...sampleSession(accessToken: 'new').user.toJson(),
            'accessToken': 'new',
            'accessTokenExpiresAtUtc': DateTime.utc(2030).toIso8601String(),
            'refreshToken': 'rotated-refresh',
            'refreshTokenExpiresAtUtc': DateTime.utc(2031).toIso8601String(),
          };
          return http.Response(jsonEncode(refreshed), 200);
        }
        protectedCalls++;
        if (request.headers['Authorization'] == 'Bearer old') {
          return http.Response('{}', 401);
        }
        expect(request.headers['Authorization'], 'Bearer new');
        return http.Response('{"value":"retried"}', 200);
      }),
    );

    final result = await client.getJson('/api/private');

    expect(result['value'], 'retried');
    expect(protectedCalls, 2);
    expect(storage.session?.accessToken, 'new');
  });
}
