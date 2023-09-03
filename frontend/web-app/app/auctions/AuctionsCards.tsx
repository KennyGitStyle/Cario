import React from 'react';
import CountdownTimer from './CountdownTimer';
import CarImage from './CarImage';
import { Auction } from '@/app/types';

type Props = {
  auction: Auction;
};

export default function AuctionsCards({ auction }: Props) {
  return (
    <a href="">
    <div className="w-full bg-gray-200 aspect-w-16 aspect-h-10 rounded-lg overflow-hidden lg:w-auto relative">
      <div> 
        <CarImage imageUrl={auction.imageUrl} />
        <div className='absolute bottom-2 left-2'>
          <CountdownTimer auctionEnd={auction.auctionEnd}/>
        </div>
      </div>
      
    </div>
    <div className='flex justify-between items-center mt-4 px-2'>
          <h3 className="text-gray-700 text-sm sm:text-base">{auction.make} {auction.model}</h3>
          <p className="font-semibold text-xs sm:text-sm">{auction.year}</p>
    </div>
    </a>
  );
}
