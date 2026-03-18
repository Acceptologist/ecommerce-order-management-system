import { Injectable, signal, computed } from '@angular/core';
import { CartItem } from '../models/cart.model';
import { Product } from '../models/product.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class CartService {
  private readonly CART_KEY = environment.cartKey;
  
  // State
  private itemsSignal = signal<CartItem[]>(this.loadFromLocalStorage());

  // Selectors
  readonly items = this.itemsSignal.asReadonly();
  
  readonly totalCount = computed(() => {
    return this.itemsSignal().reduce((sum, item) => sum + item.quantity, 0);
  });

  readonly subtotal = computed(() => {
    return this.itemsSignal().reduce((sum, item) => sum + item.product.price * item.quantity, 0);
  });

  readonly discount = computed(() => {
    let perProductDiscount = 0;
    this.itemsSignal().forEach(item => {
      if (item.product.discountRate > 0) {
        perProductDiscount += (item.product.price * item.quantity) * (item.product.discountRate / 100);
      }
    });
    return Math.round(perProductDiscount * 100) / 100;
  });

  readonly total = computed(() => {
    return Math.round((this.subtotal() - this.discount()) * 100) / 100;
  });

  constructor() {
    // Restore cart from localStorage on service init
    this.loadFromLocalStorage();
  }

  addItem(product: Product, quantity: number = 1): void {
    if (quantity <= 0) return;
    
    this.itemsSignal.update(current => {
      const existingIndex = current.findIndex(item => item.product.id === product.id);
      let updated: CartItem[];
      
      if (existingIndex >= 0) {
        updated = [...current];
        updated[existingIndex] = {
          ...updated[existingIndex],
          quantity: updated[existingIndex].quantity + quantity
        };
      } else {
        updated = [...current, { product, quantity }];
      }
      
      this.saveToLocalStorage(updated);
      return updated;
    });
  }

  removeItem(productId: number): void {
    this.itemsSignal.update(current => {
      const updated = current.filter(item => item.product.id !== productId);
      this.saveToLocalStorage(updated);
      return updated;
    });
  }

  updateQuantity(productId: number, quantity: number): void {
    if (quantity <= 0) {
      this.removeItem(productId);
      return;
    }
    
    this.itemsSignal.update(current => {
      const updated = current.map(item =>
        item.product.id === productId ? { ...item, quantity } : item
      );
      this.saveToLocalStorage(updated);
      return updated;
    });
  }

  clear(): void {
    this.itemsSignal.set([]);
    localStorage.removeItem(this.CART_KEY);
  }

  private saveToLocalStorage(items: CartItem[]): void {
    try {
      const serialized = items.map(item => ({
        product: item.product,
        quantity: item.quantity
      }));
      localStorage.setItem(this.CART_KEY, JSON.stringify(serialized));
    } catch (error) {
      console.error('Failed to save cart to localStorage:', error);
    }
  }

  private loadFromLocalStorage(): CartItem[] {
    try {
      const saved = localStorage.getItem(this.CART_KEY);
      if (saved) {
        return JSON.parse(saved);
      }
    } catch (error) {
      console.error('Failed to load cart from localStorage:', error);
    }
    return [];
  }
}
