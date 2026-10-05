abstract final class AppConfig {
  static const apiBaseUrl = String.fromEnvironment(
    'NEXTROLEAI_API_BASE_URL',
    defaultValue: 'http://10.0.2.2:5251',
  );

  static const maximumCvBytes = 5 * 1024 * 1024;
}
