import React from 'react';

export default function InputBox({ id, title, value, type, disabled, dataColumn, className, inputClass, onChange = f => f }) {
    return (
        <div className={className ? className : "input-item"}>
            <label className="label">{title}</label>
            <input id={id}
                type={type ? type : 'text'}
                value={value}
                disabled={disabled}
                onChange={e => onChange(e.target.value, dataColumn)}
                className={inputClass ? inputClass : ''} />
        </div>
    )
}
