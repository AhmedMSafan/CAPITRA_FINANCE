export interface WorkflowInboxTask {
  inboxId: number;
  taskId: number;
  instanceId: number;
  stepInstanceId: number;

  status: string;
  assignmentType?: string | null;
  assignedTo?: string | null;
  assignedRole?: string | null;
  claimedBy?: string | null;
  claimedByName?: string | null;
  /** The away user this task reaches you for, as their out-of-office delegate. */
  onBehalfOf?: string | null;
  isClaimedByMe?: boolean;

  subject?: string | null;
  route?: string | null;
  routeParameters?: string | null;
  allowedActions?: string | null;
  outcome?: string | null;

  createdOn: string;
  claimedOn?: string | null;
  completedOn?: string | null;

  // Display/business context used by the existing Inbox/Task UI.
  workflowCode?: string | null;
  workflowName?: string | null;
  stepCode?: string | null;
  stepName?: string | null;
  moduleCode?: string | null;
  functionCode?: string | null;
  entityType?: string | null;
  entityId?: number | null;
  entityRef?: string | null;
  clientId?: number | null;
  clientName?: string | null;
  amount?: number | null;
  currencyCode?: string | null;
  priority?: string | null;
  instructions?: string | null;
  dueDate?: string | null;
  /** The step's time limit (SLA): when the reminder is due, and who had the task before it was escalated. */
  remindAt?: string | null;
  escalatedFrom?: string | null;
  escalatedAt?: string | null;
  /** A parallel approval: everyone the step went to, and how they answered. */
  approvers?: { userName: string | null; status: string; outcome: string | null; answeredAt: string | null }[] | null;

  // Compatibility aliases for the current Vue pages while the API is migrated.
  assignedUserId?: string | null;
  assignedUserName?: string | null;
  assignedEmployeeNo?: string | null;
  assignedGroup?: string | null;
  accessReason?: string | null;
  applicationRoute?: string | null;
  applicationParameters?: string | null;
  createdDate?: string | null;
}

export interface CompleteWorkflowTaskRequest {
  action?: string | null;
  comment?: string | null;
  routingMode?: 'DEFAULT' | 'USER';
  assignedUserId?: string | null;
  /** The step form's values, for a step worked on its form. */
  values?: Record<string, string | null>;
}
