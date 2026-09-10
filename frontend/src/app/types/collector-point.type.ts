type CollectorPointStatus = 'approved' | 'pending' | 'denied';

type CollectorPoint = {
  id: number;
  photo_url: string;
  collectors: Collector[];
  address: Address;
  reports: Report[];
  is_active: boolean;
  status: CollectorPointStatus;
};

type CreateCollectorPointDTO = Pick<CollectorPoint, 'is_active' | 'status'> & {
  address: CreateAddressDTO;
  collectors: number[]; // IDs dos coletores
};
