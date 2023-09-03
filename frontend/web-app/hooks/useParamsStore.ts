import { createWithEqualityFn } from 'zustand/traditional';

type State = {
  pageNumber: number;
  pageSize: number;
  pageCount: number;
  searchTerm: string;
  searchValue: string;
  orderBy: string;
  filterBy: string
};

type Actions = {
  setParams: (params: Partial<State>) => void;
  reset: () => void;
  setSearchVal: (value: string) => void;
};

const initialState: State = {
  pageNumber: 1,
  pageSize: 12,
  pageCount: 1,
  searchTerm: '',
  searchValue: '',
  orderBy: 'make',
  filterBy: 'live'
};

const defaultEqualityFn = <U>(a: U, b: U) => a === b; // Default equality function

export const useParamsStore = createWithEqualityFn<State & Actions>(
  (set) => ({
    ...initialState,

    setParams: (newParams: Partial<State>) => {
      set((state) => {
        if (newParams.pageNumber) {
          return { ...state, pageNumber: newParams.pageNumber };
        } else {
          return { ...state, ...newParams, pageNumber: 1 };
        }
      });
    },

    reset: () => set(initialState),

    setSearchVal: (value: string) => {
        set({searchValue: value})
    }
  }),
  defaultEqualityFn // Passing the equality function here
);