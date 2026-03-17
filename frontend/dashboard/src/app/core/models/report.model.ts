export interface ReportMetric {
  key: string;
  value: number;
  change?: number;
  changePercent?: number;
  timestamp?: string;
}

export interface Report {
  id: string;
  type: 'account' | 'agent' | 'inbox' | 'team' | 'label';
  metrics: ReportMetric[];
  groupBy: 'day' | 'week' | 'month' | 'year';
  since: string;
  until: string;
  data: {
    labels: string[];
    datasets: {
      label: string;
      data: number[];
    }[];
  };
}
