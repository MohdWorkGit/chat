export interface AutomationRule {
  id: number;
  accountId: number;
  name: string;
  description?: string;
  eventName: string;
  conditions: AutomationCondition[];
  conditionOperator: 'AND' | 'OR';
  actions: AutomationAction[];
  active: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface AutomationCondition {
  attributeKey: string;
  filterOperator: string;
  values: string[];
  queryOperator?: string;
}

export interface AutomationAction {
  actionName: string;
  actionParams: string[];
}
