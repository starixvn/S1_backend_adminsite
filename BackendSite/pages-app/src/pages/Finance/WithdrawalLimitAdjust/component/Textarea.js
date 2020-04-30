import React from 'react';

export const TextArea = (props)=>{
    return(
        <div className="input-item" style={{marginTop:'.5rem'}}>
                <span className="label">{props.displayText}</span>
                <div className="field">
                    <textarea 
                        rows="3" 
                        cols="100"
                        value={props.Value}
                        onChange={(e)=>{props.onBlur(e.target.value, props.dataColumn)}}
                    />
                </div>
        </div>
    )
}