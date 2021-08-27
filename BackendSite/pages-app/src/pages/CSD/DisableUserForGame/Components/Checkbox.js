import React, {useState} from 'react'

function Checkbox({ title, value, checked, toggle = f => f}) {

    return (
        <label className="checkbox checkbox-checked" >
            <input type="checkbox" value={value} checked={checked} onChange={() => toggle(value, checked)} />
            <i className="icon icon-checkbox"></i>
            <span className="text">{title}</span>
        </label>
    );
}

export default Checkbox