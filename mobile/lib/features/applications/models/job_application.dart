class ApplicationStatusEvent {
  const ApplicationStatusEvent({
    required this.id,
    required this.previousStatus,
    required this.newStatus,
    required this.actorRole,
    required this.note,
    required this.createdAtUtc,
  });

  factory ApplicationStatusEvent.fromJson(Map<String, dynamic> json) =>
      ApplicationStatusEvent(
        id: json['id'] as String,
        previousStatus: json['previousStatus'] as String?,
        newStatus: json['newStatus'] as String,
        actorRole: json['actorRole'] as String,
        note: json['note'] as String,
        createdAtUtc: DateTime.parse(json['createdAtUtc'] as String),
      );

  final String id;
  final String? previousStatus;
  final String newStatus;
  final String actorRole;
  final String note;
  final DateTime createdAtUtc;
}

class NotificationDelivery {
  const NotificationDelivery({
    required this.eventType,
    required this.provider,
    required this.status,
    required this.attemptCount,
  });

  factory NotificationDelivery.fromJson(Map<String, dynamic> json) =>
      NotificationDelivery(
        eventType: json['eventType'] as String,
        provider: json['provider'] as String,
        status: json['status'] as String,
        attemptCount: json['attemptCount'] as int,
      );

  final String eventType;
  final String provider;
  final String status;
  final int attemptCount;
}

class JobApplication {
  const JobApplication({
    required this.id,
    required this.jobPostingId,
    required this.jobTitle,
    required this.companyName,
    required this.jobLocation,
    required this.coverNote,
    required this.status,
    required this.sourceWorkflowRunId,
    required this.statusHistory,
    required this.notificationDeliveries,
    required this.createdAtUtc,
    required this.updatedAtUtc,
  });

  factory JobApplication.fromJson(Map<String, dynamic> json) => JobApplication(
    id: json['id'] as String,
    jobPostingId: json['jobPostingId'] as String,
    jobTitle: json['jobTitle'] as String,
    companyName: json['companyName'] as String,
    jobLocation: json['jobLocation'] as String,
    coverNote: json['coverNote'] as String,
    status: json['status'] as String,
    sourceWorkflowRunId: json['sourceWorkflowRunId'] as String?,
    statusHistory: (json['statusHistory'] as List<dynamic>)
        .map(
          (item) =>
              ApplicationStatusEvent.fromJson(item as Map<String, dynamic>),
        )
        .toList(growable: false),
    notificationDeliveries: (json['notificationDeliveries'] as List<dynamic>)
        .map(
          (item) => NotificationDelivery.fromJson(item as Map<String, dynamic>),
        )
        .toList(growable: false),
    createdAtUtc: DateTime.parse(json['createdAtUtc'] as String),
    updatedAtUtc: DateTime.parse(json['updatedAtUtc'] as String),
  );

  final String id;
  final String jobPostingId;
  final String jobTitle;
  final String companyName;
  final String jobLocation;
  final String coverNote;
  final String status;
  final String? sourceWorkflowRunId;
  final List<ApplicationStatusEvent> statusHistory;
  final List<NotificationDelivery> notificationDeliveries;
  final DateTime createdAtUtc;
  final DateTime updatedAtUtc;
}
