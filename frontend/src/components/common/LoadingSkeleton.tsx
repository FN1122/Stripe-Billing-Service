import React from 'react';
import './LoadingSkeleton.scss';

interface LoadingSkeletonProps {
  height?: number;
  width?: string;
  count?: number;
  circle?: boolean;
}

export const LoadingSkeleton: React.FC<LoadingSkeletonProps> = ({
  height = 20,
  width = '100%',
  count = 1,
  circle = false,
}) => {
  return (
    <>
      {Array.from({ length: count }).map((_, i) => (
        <div
          key={i}
          className={`loading-skeleton ${circle ? 'circle' : ''}`}
          style={{ height, width, marginBottom: '10px' }}
        />
      ))}
    </>
  );
};
