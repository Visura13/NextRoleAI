class JobSeekerProfile {
  const JobSeekerProfile({
    required this.headline,
    required this.summary,
    required this.location,
    required this.preferredJobTitle,
    required this.yearsOfExperience,
    required this.skills,
  });

  final String headline;
  final String summary;
  final String location;
  final String preferredJobTitle;
  final int yearsOfExperience;
  final List<String> skills;

  factory JobSeekerProfile.empty() => const JobSeekerProfile(
    headline: '',
    summary: '',
    location: '',
    preferredJobTitle: '',
    yearsOfExperience: 0,
    skills: [],
  );

  factory JobSeekerProfile.fromJson(Map<String, dynamic> json) =>
      JobSeekerProfile(
        headline: json['headline'] as String,
        summary: json['summary'] as String,
        location: json['location'] as String,
        preferredJobTitle: json['preferredJobTitle'] as String,
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
    'yearsOfExperience': yearsOfExperience,
    'skills': skills,
  };
}
