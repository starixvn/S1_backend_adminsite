import React from 'react';

const SelectBox = ({Options, HandleChange, SelectedValue, dataColumn, Disabled})=>{
    const OnChange=(e)=>{
        HandleChange(e.target.value, dataColumn);
    }
    return(
        <div className="input-item">
            <select className="field" value={SelectedValue} onChange={OnChange} disabled={Disabled}>
                {
                    Options && Options.map(o=>{
                        return <option key={o.value} value={o.value}>{o.displayText}</option>
                    })
                }
            </select>
        </div>
    )
}

export default SelectBox;