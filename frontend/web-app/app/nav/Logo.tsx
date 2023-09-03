'use client'

import { useParamsStore } from '@/hooks/useParamsStore'
import React from 'react'
import { AiFillCar } from 'react-icons/ai'

export default function Logo() {
    const reset = useParamsStore(state => state.reset)
    return (
      <div onClick={reset} className="cursor-pointer sm:text-base lg:text-lg flex items-center gap-2 text-3xl font-semibold text-green-800">
          <AiFillCar size={34} />
          <div>Carios Auctions</div>
      </div>
    )
}
