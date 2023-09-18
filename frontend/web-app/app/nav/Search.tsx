'use client'

import { useParamsStore } from '@/hooks/useParamsStore';
import { usePathname, useRouter } from 'next/navigation';
import React, { useState } from 'react';
import { FaSearch } from 'react-icons/fa';

export default function Search() {
    const router = useRouter()
    const pathname = usePathname()
    const setParams = useParamsStore(state => state.setParams)
    const setSearchValue = useParamsStore(state => state.setSearchVal)
    const searchValue = useParamsStore(state => state.searchValue)

    function onCharge(event: any)
    {
        setSearchValue(event.target.value)
    }

    function search() {
      if(pathname !== '/') router.push('/')
        setParams({searchTerm: searchValue})
    }

    return (
      <div className="flex w-full md:w-[50%] items-center border-2 rounded-full py-2 shadow-sm">
        <input
          onKeyDown={(e: any) => {
            if(e.key === 'Enter') Search()
          }}
          value={searchValue}
          onChange={onCharge}
          type="text"
          placeholder="Search for cars by make, model, or color"
          className="
            input-custom 
            text-sm
            text-gray-600
          "
        />
        <button className="flex-none" onClick={search}>
          <FaSearch
            size={40}  // Increase the size here
            className="bg-green-600 text-white rounded-full p-2 cursor-pointer mx-1"  // Adjust the padding here
          />
        </button>
      </div>
    );
}
