
export interface BusinessCard {
  id: number;
  name: string;
  gender: 'Male' | 'Female' | 'Other' | '';
  dateOfBirth: string; // YYYY-MM-DD
  email: string;
  phone: string;
  photo?: string; // Base64 encoded string
  address: string;
}
