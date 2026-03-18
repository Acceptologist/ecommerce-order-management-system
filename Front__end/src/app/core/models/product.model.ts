export interface Product {
  id: number;
  name: string;
  description: string;
  price: number;
  stockQuantity: number;
  categoryName: string;
  categoryId: number;
  imageUrl?: string | null;
  discountRate: number;
}

export interface PagedResult<T> { items: T[]; totalCount: number; page: number; pageSize: number; }

export interface CreateProductDto {
  name: string;
  description: string;
  price: number;
  stockQuantity: number;
  categoryId: number;
  imageUrl?: string | null;
  discountRate: number;
}

export interface UpdateProductDto {
  name: string;
  description: string;
  price: number;
  stockQuantity: number;
  categoryId: number;
  imageUrl?: string | null;
  discountRate: number;
}
