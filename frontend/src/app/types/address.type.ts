type Address = {
  id: number;
  road?: string;
  neighborhood?: string;
  complement?: string;
  city?: string;
  lat?: number;
  lng?: number;
};

type CreateAddressDTO = Omit<Address, 'id'>;
