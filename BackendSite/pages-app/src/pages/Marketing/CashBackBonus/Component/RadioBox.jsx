import React from 'react';

const RadioBtn = ({Id, DisplayText, Value, Name, OnChange, Checked, Disabled})=>{
    return(
        <div className="custom-control custom-radio custom-control-inline">
            <input type="radio" id={Id} name={Name} className="custom-control-input revert-style" value={Value} onChange={OnChange} checked={Checked} disabled={Disabled}/>
            <label className="custom-control-label" htmlFor={Id}>{DisplayText}</label>
        </div>
    )
}

export default RadioBtn;