import EmptyFilter from '@/app/components/EmptyFilter'
import React from 'react'

export default function Page({searchParams}: {searchParams: {callbackUrl: string}}) {
  return (
    <EmptyFilter
        title='You need to be logged in to beable to access this feature'
        subtitle='Please click below to signin'
        showLogin
        callbackUrl={searchParams.callbackUrl}
    />
  )
}
