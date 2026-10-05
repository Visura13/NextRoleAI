class CurrentUser {
  const CurrentUser({
    required this.userId,
    required this.email,
    required this.firstName,
    required this.lastName,
    required this.role,
  });

  final String userId;
  final String email;
  final String firstName;
  final String lastName;
  final String role;

  String get fullName => '$firstName $lastName'.trim();

  factory CurrentUser.fromJson(Map<String, dynamic> json) => CurrentUser(
    userId: json['userId'] as String,
    email: json['email'] as String,
    firstName: json['firstName'] as String,
    lastName: json['lastName'] as String,
    role: json['role'] as String,
  );

  Map<String, dynamic> toJson() => {
    'userId': userId,
    'email': email,
    'firstName': firstName,
    'lastName': lastName,
    'role': role,
  };
}

class AuthSession {
  const AuthSession({
    required this.user,
    required this.accessToken,
    required this.accessTokenExpiresAtUtc,
    required this.refreshToken,
    required this.refreshTokenExpiresAtUtc,
  });

  final CurrentUser user;
  final String accessToken;
  final DateTime accessTokenExpiresAtUtc;
  final String refreshToken;
  final DateTime refreshTokenExpiresAtUtc;

  factory AuthSession.fromJson(Map<String, dynamic> json) => AuthSession(
    user: CurrentUser.fromJson(json),
    accessToken: json['accessToken'] as String,
    accessTokenExpiresAtUtc: DateTime.parse(
      json['accessTokenExpiresAtUtc'] as String,
    ),
    refreshToken: json['refreshToken'] as String,
    refreshTokenExpiresAtUtc: DateTime.parse(
      json['refreshTokenExpiresAtUtc'] as String,
    ),
  );

  factory AuthSession.fromStoredJson(Map<String, dynamic> json) => AuthSession(
    user: CurrentUser.fromJson(json['user'] as Map<String, dynamic>),
    accessToken: json['accessToken'] as String,
    accessTokenExpiresAtUtc: DateTime.parse(
      json['accessTokenExpiresAtUtc'] as String,
    ),
    refreshToken: json['refreshToken'] as String,
    refreshTokenExpiresAtUtc: DateTime.parse(
      json['refreshTokenExpiresAtUtc'] as String,
    ),
  );

  AuthSession withUser(CurrentUser nextUser) => AuthSession(
    user: nextUser,
    accessToken: accessToken,
    accessTokenExpiresAtUtc: accessTokenExpiresAtUtc,
    refreshToken: refreshToken,
    refreshTokenExpiresAtUtc: refreshTokenExpiresAtUtc,
  );

  Map<String, dynamic> toJson() => {
    'user': user.toJson(),
    'accessToken': accessToken,
    'accessTokenExpiresAtUtc': accessTokenExpiresAtUtc.toIso8601String(),
    'refreshToken': refreshToken,
    'refreshTokenExpiresAtUtc': refreshTokenExpiresAtUtc.toIso8601String(),
  };
}

class RegistrationInput {
  const RegistrationInput({
    required this.firstName,
    required this.lastName,
    required this.email,
    required this.password,
  });

  final String firstName;
  final String lastName;
  final String email;
  final String password;

  Map<String, dynamic> toJson() => {
    'firstName': firstName,
    'lastName': lastName,
    'email': email,
    'password': password,
  };
}
