class CvProfile {
  const CvProfile({
    required this.id,
    required this.originalFileName,
    required this.contentType,
    required this.sizeBytes,
    required this.sha256Checksum,
    required this.status,
    required this.candidateName,
    required this.email,
    required this.phone,
    required this.location,
    required this.currentJobTitle,
    required this.professionalSummary,
    required this.yearsExperience,
    required this.skills,
    required this.updatedAtUtc,
  });

  final String id;
  final String originalFileName;
  final String contentType;
  final int sizeBytes;
  final String sha256Checksum;
  final String status;
  final String candidateName;
  final String email;
  final String phone;
  final String location;
  final String currentJobTitle;
  final String professionalSummary;
  final int yearsExperience;
  final List<String> skills;
  final DateTime updatedAtUtc;

  bool get isConfirmed => status == 'Confirmed';

  factory CvProfile.fromJson(Map<String, dynamic> json) => CvProfile(
    id: json['id'] as String,
    originalFileName: json['originalFileName'] as String,
    contentType: json['contentType'] as String,
    sizeBytes: json['sizeBytes'] as int,
    sha256Checksum: json['sha256Checksum'] as String,
    status: json['status'] as String,
    candidateName: json['candidateName'] as String,
    email: json['email'] as String,
    phone: json['phone'] as String,
    location: json['location'] as String,
    currentJobTitle: json['currentJobTitle'] as String,
    professionalSummary: json['professionalSummary'] as String,
    yearsExperience: json['yearsExperience'] as int,
    skills: (json['skills'] as List<dynamic>)
        .map((item) => item.toString())
        .toList(),
    updatedAtUtc: DateTime.parse(json['updatedAtUtc'] as String),
  );
}

class CvProfileUpdate {
  const CvProfileUpdate({
    required this.candidateName,
    required this.email,
    required this.phone,
    required this.location,
    required this.currentJobTitle,
    required this.professionalSummary,
    required this.yearsExperience,
    required this.skills,
  });

  final String candidateName;
  final String email;
  final String phone;
  final String location;
  final String currentJobTitle;
  final String professionalSummary;
  final int yearsExperience;
  final List<String> skills;

  Map<String, dynamic> toJson() => {
    'candidateName': candidateName,
    'email': email,
    'phone': phone,
    'location': location,
    'currentJobTitle': currentJobTitle,
    'professionalSummary': professionalSummary,
    'yearsExperience': yearsExperience,
    'skills': skills,
  };
}
