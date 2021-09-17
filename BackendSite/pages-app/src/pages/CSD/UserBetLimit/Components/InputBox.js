import React from 'react';

export default function InputBox ({ id ,title, value, type, disabled, onChange, dataColumn }) {
    let inputKey = `${dataColumn}-${id}`;
    return (
        <div className="input-item">
            <label className="label">{title}</label>
            <input id={inputKey} 
                   type={type ? type : 'text'} 
                   value={value} 
                   disabled={disabled} 
                   onChange={e => onChange(e.target.value, inputKey)}/>
        </div>
    )
}
