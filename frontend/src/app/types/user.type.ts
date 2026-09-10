export type User = {
  id: number;
  email: string;
  user_type: 'email' | 'google';
  is_active: boolean;
  created_at: string;
  updated_at: string;
};
