import React from 'react';
import ReactDOM from 'react-dom';
import EditPanel from './Edit';

const Panel = ()=>{
    return(
        <div className="container-fluid">
            <h4 className="page-name">Personal Message Batch</h4>
            <EditPanel />
        </div>
    )
}

export default Panel;

if(process.env.NODE_ENV==='production'){
    ReactDOM.render(<Panel />, document.getElementById('content'))
}