// Import necessary modules and types
import { Auction, PagedResult } from "@/types"; // Replace with your actual path and types
import { createWithEqualityFn } from "zustand/traditional";

// Define the State type
type State = {
  auctions: Auction[];
  totalCount: number;
  pageCount: number;
};

// Define the Actions type
type Actions = {
  setData: (data: PagedResult<Auction>) => void;
  setCurrentPrice: (auctionId: string, amount: number) => void;
};

// Merge State and Actions into a StoreState type
type StoreState = State & Actions;

// Define the initial state
const initialState: State = {
  auctions: [],
  pageCount: 0,
  totalCount: 0,
};

// Define the equality function for Zustand
const equalityFn = <U>(a: U, b: U): boolean => {
  if (typeof a === 'object' && typeof b === 'object') {
    const stateA = a as unknown as StoreState;
    const stateB = b as unknown as StoreState;

    return Object.keys(stateB).every(key => {
      return stateA[key as keyof StoreState] === stateB[key as keyof StoreState];
    });
  }
  return a === b;
};

// Create the Zustand store
export const useAuctionStore = createWithEqualityFn<StoreState>((set) => ({
  // Spread the initial state
  ...initialState,
  
  // Define the setData action
  setData: (data: PagedResult<Auction>) => {
    set(() => ({
      auctions: data.results,
      totalCount: data.totalCount,
      pageCount: data.pageCount,
    }));
  },

  // Define the setCurrentPrice action
  setCurrentPrice: (auctionId: string, amount: number) => {
    set((state) => ({
      auctions: state.auctions.map((auction) =>
        auction.id === auctionId ? { ...auction, currentHighBid: amount } : auction
      ),
    }));
  },
}), equalityFn);
