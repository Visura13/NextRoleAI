class JobSkill {
  const JobSkill({required this.name, required this.isRequired});

  final String name;
  final bool isRequired;

  factory JobSkill.fromJson(Map<String, dynamic> json) => JobSkill(
    name: json['name'] as String,
    isRequired: json['isRequired'] as bool,
  );
}

class Job {
  const Job({
    required this.id,
    required this.companyName,
    required this.title,
    required this.description,
    required this.location,
    required this.employmentType,
    required this.workMode,
    required this.minimumYearsExperience,
    required this.salaryMinimum,
    required this.salaryMaximum,
    required this.salaryCurrency,
    required this.closesAtUtc,
    required this.skills,
  });

  final String id;
  final String companyName;
  final String title;
  final String description;
  final String location;
  final String employmentType;
  final String workMode;
  final int minimumYearsExperience;
  final num? salaryMinimum;
  final num? salaryMaximum;
  final String? salaryCurrency;
  final DateTime? closesAtUtc;
  final List<JobSkill> skills;

  factory Job.fromJson(Map<String, dynamic> json) => Job(
    id: json['id'] as String,
    companyName: json['companyName'] as String,
    title: json['title'] as String,
    description: json['description'] as String,
    location: json['location'] as String,
    employmentType: json['employmentType'] as String,
    workMode: json['workMode'] as String,
    minimumYearsExperience: json['minimumYearsExperience'] as int,
    salaryMinimum: json['salaryMinimum'] as num?,
    salaryMaximum: json['salaryMaximum'] as num?,
    salaryCurrency: json['salaryCurrency'] as String?,
    closesAtUtc: json['closesAtUtc'] == null
        ? null
        : DateTime.parse(json['closesAtUtc'] as String),
    skills: (json['skills'] as List<dynamic>)
        .map((item) => JobSkill.fromJson(item as Map<String, dynamic>))
        .toList(growable: false),
  );
}

class PagedJobs {
  const PagedJobs({
    required this.items,
    required this.page,
    required this.pageSize,
    required this.totalCount,
    required this.totalPages,
  });

  final List<Job> items;
  final int page;
  final int pageSize;
  final int totalCount;
  final int totalPages;

  factory PagedJobs.fromJson(Map<String, dynamic> json) => PagedJobs(
    items: (json['items'] as List<dynamic>)
        .map((item) => Job.fromJson(item as Map<String, dynamic>))
        .toList(growable: false),
    page: json['page'] as int,
    pageSize: json['pageSize'] as int,
    totalCount: json['totalCount'] as int,
    totalPages: json['totalPages'] as int,
  );
}

class JobFilters {
  const JobFilters({
    this.search = '',
    this.location = '',
    this.employmentType = '',
    this.workMode = '',
    this.sortBy = 'newest',
  });

  final String search;
  final String location;
  final String employmentType;
  final String workMode;
  final String sortBy;

  Map<String, String?> toQuery({required int page, int pageSize = 10}) => {
    'search': search.trim(),
    'location': location.trim(),
    'employmentType': employmentType,
    'workMode': workMode,
    'sortBy': sortBy,
    'page': '$page',
    'pageSize': '$pageSize',
  };
}
