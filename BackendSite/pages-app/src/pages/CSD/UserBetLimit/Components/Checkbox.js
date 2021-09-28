import React from 'react'

export default function Checkbox({ title, value, checked, toggle = f => f}) {

    return (
        <label className="checkbox checkbox-checked" >
            <input type="checkbox" value={value} checked={checked} onChange={() => toggle(value, title, checked)} />
            <i className="icon icon-checkbox"></i>
            <span className="text">{title}</span>
        </label>
    );
}