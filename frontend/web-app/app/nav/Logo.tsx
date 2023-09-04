'use client'

import { useParamsStore } from '@/hooks/useParamsStore'
import { usePathname, useRouter } from 'next/navigation'
import React from 'react'
import { AiFillCar } from 'react-icons/ai'

export default function Logo() {
  const router = useRouter()
  const pathname = usePathname()
  const reset = useParamsStore(state => state.reset)

    
  function doReset() {
    if(pathname !== '/') router.push('/')
    reset()
  }

    
  return (
    <div onClick={doReset} className="cursor-pointer sm:text-base lg:text-lg flex items-center gap-2 text-3xl font-semibold text-green-800">
        <AiFillCar size={34} />
        <div>Carios Auctions</div>
    </div>
  )
}
