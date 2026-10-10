import '../../jobs/models/job_models.dart';

class MatchBreakdown {
  const MatchBreakdown({
    required this.skillsFit,
    required this.roleFit,
    required this.experienceFit,
    required this.educationFit,
    required this.locationFit,
    required this.salaryFit,
  });

  final num skillsFit;
  final num roleFit;
  final num experienceFit;
  final num educationFit;
  final num locationFit;
  final num salaryFit;

  factory MatchBreakdown.fromJson(Map<String, dynamic> json) => MatchBreakdown(
    skillsFit: json['skillsFit'] as num,
    roleFit: json['roleFit'] as num,
    experienceFit: json['experienceFit'] as num,
    educationFit: json['educationFit'] as num,
    locationFit: json['locationFit'] as num,
    salaryFit: json['salaryFit'] as num,
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
