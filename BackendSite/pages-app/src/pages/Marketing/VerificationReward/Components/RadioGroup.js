import React from 'react';
import RadioButton from './RadioButton'

export default function RadioGroup ({ title, options, selected, mainKey = null, disabled = false, toggle = f => f }) {
    let opt = options.map((o, oIdx)=>{
        let key = `${title}-${o.value}${mainKey === null ? '' : `-${mainKey}`}`;
        return <RadioButton 
                    key={key}
                    id={key}
                    title={o.displayText}
                    value={o.value}
                    checked={selected === o.value}
                    disabled={disabled}
                    toggle={toggle} />
        });
    return(
        <>
            <div className="row-flex">
                <div className="input-item">
                    <label className="label">{title}</label>
                </div>
            </div>
            <div className="row-flex">{opt}</div>
        </>
    )
}
