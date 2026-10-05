import '../../jobs/models/job_models.dart';

class MatchBreakdown {
  const MatchBreakdown({
    required this.requiredSkills,
    required this.preferredSkills,
    required this.title,
    required this.experience,
    required this.location,
  });

  final num requiredSkills;
  final num preferredSkills;
  final num title;
  final num experience;
  final num location;

  factory MatchBreakdown.fromJson(Map<String, dynamic> json) => MatchBreakdown(
    requiredSkills: json['requiredSkills'] as num,
    preferredSkills: json['preferredSkills'] as num,
    title: json['title'] as num,
    experience: json['experience'] as num,
    location: json['location'] as num,
  );
}

class JobRecommendation {
  const JobRecommendation({
    required this.job,
    required this.score,
    required this.breakdown,
    required this.matchedSkills,
    required this.missingRequiredSkills,
    required this.reasons,
    required this.algorithmVersion,
  });

  final Job job;
  final num score;
  final MatchBreakdown breakdown;
  final List<String> matchedSkills;
  final List<String> missingRequiredSkills;
  final List<String> reasons;
  final String algorithmVersion;

  factory JobRecommendation.fromJson(Map<String, dynamic> json) =>
      JobRecommendation(
        job: Job.fromJson(json['job'] as Map<String, dynamic>),
        score: json['score'] as num,
        breakdown: MatchBreakdown.fromJson(
          json['breakdown'] as Map<String, dynamic>,
        ),
        matchedSkills: (json['matchedSkills'] as List<dynamic>)
            .map((item) => item.toString())
            .toList(),
        missingRequiredSkills: (json['missingRequiredSkills'] as List<dynamic>)
            .map((item) => item.toString())
            .toList(),
        reasons: (json['reasons'] as List<dynamic>)
            .map((item) => item.toString())
            .toList(),
        algorithmVersion: json['algorithmVersion'] as String,
      );
}
