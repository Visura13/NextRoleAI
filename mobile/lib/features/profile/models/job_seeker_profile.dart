class JobSeekerProfile {
  const JobSeekerProfile({
    required this.headline,
    required this.summary,
    required this.location,
    required this.preferredJobTitle,
    required this.preferredSalary,
    required this.yearsOfExperience,
    required this.skills,
  });

  final String headline;
  final String summary;
  final String location;
  final String preferredJobTitle;
  final num? preferredSalary;
  final int yearsOfExperience;
  final List<String> skills;

  factory JobSeekerProfile.empty() => const JobSeekerProfile(
    headline: '',
    summary: '',
    location: '',
    preferredJobTitle: '',
    preferredSalary: null,
    yearsOfExperience: 0,
    skills: [],
  );

  factory JobSeekerProfile.fromJson(Map<String, dynamic> json) =>
      JobSeekerProfile(
        headline: json['headline'] as String,
        summary: json['summary'] as String,
        location: json['location'] as String,
        preferredJobTitle: json['preferredJobTitle'] as String,
        preferredSalary: json['preferredSalary'] as num?,
        yearsOfExperience: json['yearsOfExperience'] as int,
        skills: (json['skills'] as List<dynamic>)
            .map((skill) => skill.toString())
            .toList(growable: false),
      );

  Map<String, dynamic> toJson() => {
    'headline': headline,
    'summary': summary,
    'location': location,
    'preferredJobTitle': preferredJobTitle,
    'preferredSalary': preferredSalary,
    'yearsOfExperience': yearsOfExperience,
    'skills': skills,
  };
}
