import React from 'react';
import './JsonViewer.scss';

interface JsonViewerProps {
  data: any;
  title?: string;
}

export const JsonViewer: React.FC<JsonViewerProps> = ({ data, title }) => {
  return (
    <div className="json-viewer">
      {title && <h4 className="json-title">{title}</h4>}
      <pre className="json-content">{JSON.stringify(data, null, 2)}</pre>
    </div>
  );
};
