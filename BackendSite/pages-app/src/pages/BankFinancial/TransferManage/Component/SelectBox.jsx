import React from 'react';

const SelectBox = ({Options, HandleChange, SelectedValue, dataColumn, Disabled, Title, iptSubClass})=>{
    const OnChange=(e)=>{
        HandleChange(e.target.value, dataColumn);
    }
    return(
        <div className="input-item">
            {
                Title ? <label className="label">{Title}</label> : null
            }
            <div className={`field${iptSubClass ? (' '+iptSubClass) : ''}`}>
            <select className="field" value={SelectedValue} onChange={OnChange} disabled={Disabled} style={{ width: "100%"}}>
                {
                    Options && Options.map(o=>{
                        return <option key={o.value} value={o.value}>{o.displayText}</option>
                    })
                }
            </select>
            </div>
        </div>
    )
}

export default SelectBox;