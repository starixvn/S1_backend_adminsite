import React from 'react';

export default function RadioButton({ id, title, value, name, checked, disabled, toggle = f => f}) {

    return (
        <div className="custom-control custom-radio custom-control-inline">
            <input type="radio" id={id} className="custom-control-input" name="siteList" value={value} onChange={() => toggle(value)} checked={checked} disabled={disabled}/>
            <label className="custom-control-label" htmlFor={id}>{title}</label>
        </div>
    )
}