import React from 'react';

export default function InputBox ({ id ,title, value, type, disabled, onChange, dataColumn }) {
    let inputKey = `${dataColumn}-${id}`;
    return (
        <div className="input-item">
            <label className="label">{title}</label>
            <input id={id} 
                   type={type ? type : 'text'} 
                   value={value} 
                   disabled={disabled} 
                   onChange={e => onChange(e.target.value, dataColumn)}/>
        </div>
    )
}
