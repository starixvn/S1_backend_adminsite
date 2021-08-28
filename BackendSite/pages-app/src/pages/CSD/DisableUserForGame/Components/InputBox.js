import React from 'react';

export default function InputBox ({title, value, disabled, onChange}) {

    return(
        <div className="input-item">
            <label className="label">{title}</label>
            <div>
                <input type='text' value={value} disabled={disabled} onChange={event => onChange(event.target.value)}/>
            </div>
        </div>
    )
}
