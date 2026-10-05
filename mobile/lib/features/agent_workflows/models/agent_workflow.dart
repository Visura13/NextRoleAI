class AgentToolCall {
  const AgentToolCall({
    required this.toolName,
    required this.succeeded,
    required this.durationMilliseconds,
  });

  final String toolName;
  final bool succeeded;
  final int durationMilliseconds;

  factory AgentToolCall.fromJson(Map<String, dynamic> json) => AgentToolCall(
    toolName: json['toolName'] as String,
    succeeded: json['succeeded'] as bool,
    durationMilliseconds: json['durationMilliseconds'] as int,
  );
}

class AgentWorkflowStep {
  const AgentWorkflowStep({
    required this.agentName,
    required this.responsibility,
    required this.status,
    required this.retryCount,
    required this.durationMilliseconds,
    required this.allowedTools,
    required this.toolCalls,
  });

  final String agentName;
  final String responsibility;
  final String status;
  final int retryCount;
  final int? durationMilliseconds;
  final List<String> allowedTools;
  final List<AgentToolCall> toolCalls;

  factory AgentWorkflowStep.fromJson(Map<String, dynamic> json) =>
      AgentWorkflowStep(
        agentName: json['agentName'] as String,
        responsibility: json['responsibility'] as String,
        status: json['status'] as String,
        retryCount: json['retryCount'] as int,
        durationMilliseconds: json['durationMilliseconds'] as int?,
        allowedTools: (json['allowedTools'] as List<dynamic>)
            .map((item) => item.toString())
            .toList(),
        toolCalls: (json['toolCalls'] as List<dynamic>)
            .map((item) => AgentToolCall.fromJson(item as Map<String, dynamic>))
            .toList(),
      );
}

class AgentValidation {
  const AgentValidation({
    required this.ruleName,
    required this.passed,
    required this.message,
  });

  final String ruleName;
  final bool passed;
  final String message;

  factory AgentValidation.fromJson(Map<String, dynamic> json) =>
      AgentValidation(
        ruleName: json['ruleName'] as String,
        passed: json['passed'] as bool,
        message: json['message'] as String,
      );
}

class AgentShortlistItem {
  const AgentShortlistItem({
    required this.jobId,
    required this.companyName,
    required this.title,
    required this.location,
    required this.workMode,
    required this.score,
    required this.rank,
    required this.reasonSummary,
    required this.isApproved,
  });

  final String jobId;
  final String companyName;
  final String title;
  final String location;
  final String workMode;
  final num score;
  final int rank;
  final String reasonSummary;
  final bool isApproved;

  factory AgentShortlistItem.fromJson(Map<String, dynamic> json) =>
      AgentShortlistItem(
        jobId: json['jobId'] as String,
        companyName: json['companyName'] as String,
        title: json['title'] as String,
        location: json['location'] as String,
        workMode: json['workMode'] as String,
        score: json['score'] as num,
        rank: json['rank'] as int,
        reasonSummary: json['reasonSummary'] as String,
        isApproved: json['isApproved'] as bool,
      );
}

class AgentWorkflow {
  const AgentWorkflow({
    required this.id,
    required this.objective,
    required this.status,
    required this.approvalStatus,
    required this.currentAgent,
    required this.failureCode,
    required this.failureMessage,
    required this.finalSummary,
    required this.createdAtUtc,
    required this.steps,
    required this.validationResults,
    required this.shortlist,
  });

  final String id;
  final String objective;
  final String status;
  final String approvalStatus;
  final String currentAgent;
  final String? failureCode;
  final String? failureMessage;
  final String? finalSummary;
  final DateTime createdAtUtc;
  final List<AgentWorkflowStep> steps;
  final List<AgentValidation> validationResults;
  final List<AgentShortlistItem> shortlist;

  factory AgentWorkflow.fromJson(Map<String, dynamic> json) => AgentWorkflow(
    id: json['id'] as String,
    objective: json['objective'] as String,
    status: json['status'] as String,
    approvalStatus: json['approvalStatus'] as String,
    currentAgent: json['currentAgent'] as String,
    failureCode: json['failureCode'] as String?,
    failureMessage: json['failureMessage'] as String?,
    finalSummary: json['finalSummary'] as String?,
    createdAtUtc: DateTime.parse(json['createdAtUtc'] as String),
    steps: (json['steps'] as List<dynamic>)
        .map((item) => AgentWorkflowStep.fromJson(item as Map<String, dynamic>))
        .toList(),
    validationResults: (json['validationResults'] as List<dynamic>)
        .map((item) => AgentValidation.fromJson(item as Map<String, dynamic>))
        .toList(),
    shortlist: (json['shortlist'] as List<dynamic>)
        .map(
          (item) => AgentShortlistItem.fromJson(item as Map<String, dynamic>),
        )
        .toList(),
  );
}
