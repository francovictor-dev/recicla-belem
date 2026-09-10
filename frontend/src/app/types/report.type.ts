type ReportType = 'not_exist' | 'wrong_location' | 'inconsistent_photo';

export type ReportInterface = {
  id: number;
  message: string;
  report_type: ReportType;
  user_id: number;
};

export type CreateReportDTO = Omit<ReportInterface, 'id'>;
