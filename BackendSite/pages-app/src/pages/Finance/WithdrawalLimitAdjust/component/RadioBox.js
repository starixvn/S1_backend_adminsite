import React from 'react';

export const RadioBtn = (props)=>{
    return(
        <div className="custom-control custom-radio custom-control-inline">
            <input type="radio" id={props.Id} name="Option" className="custom-control-input" value={props.value} onChange={props.OnChange} checked={props.Checked}/>
            <label className="custom-control-label" htmlFor={props.Id}>{props.displayText}</label>
        </div>
    )
}