'use client'

import React, { useState } from 'react';
import Image from 'next/image';

type Props = {
    imageUrl: string;
};

export default function CarImage({ imageUrl }: Props) {
  const [isLoading, setLoading] = useState(true);

  return (
    <div className={`relative group w-full h-full ${isLoading ? 'grayscale blur-2xl scale-110' : 'grayscale-0 blur-0 scale-100'}`}>
      <Image
        src={imageUrl}
        alt='image'
        layout='fill'
        objectFit="cover"
        priority
        onLoadingComplete={() => setLoading(false)}
      />
      <div 
        className="
          absolute inset-0 
          duration-700 ease-in-out 
          group-hover:opacity-75
        "
      ></div>
    </div>
  );
}
