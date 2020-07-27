import React, { useState } from "react";
import { api } from '../../../module/WebAPI';
export const PopBoxContentAddLevel = (props) => {
    let lastList = props.LevelList.length === 0 ? [] : props.LevelList[props.LevelList.length - 1];
    let siteId = props.SiteId, depositType = props.DepositType, currencyId = props.CurrencyId, vipLevel = 1, depositMin = 1, depositMax = 1, depositStatus = 1;

    if (lastList.length !== 0) {
        vipLevel = parseInt(lastList.vipLevel) + 1;
        depositMin = parseInt(lastList.depositMin) + 1;
        depositMax = parseInt(lastList.depositMax) + 1;
    }

    const [Level, setLevel] = useState(vipLevel);
    const [DepositMin, setDepositMin] = useState(depositMin);
    const [DepositMax, setDepositMax] = useState(depositMax);
    let newLevelList = { siteId: siteId, depositType: depositType, currencyId: currencyId, vipLevel: Level, depositMin: DepositMin, depositMax: DepositMax, depositStatus: depositStatus }
    const handleSave = (e) => {
        e.preventDefault();

        if (lastList.vipLevel >= Level) { return (alert("err1")) };
        if (lastList.depositMin >= DepositMin) { return (alert("Deposit Min too small")) };
        if (lastList.depositMax >= DepositMax) { return (alert("Deposit Max too small")) };
        api.post('/api/Deposit/AddDepositLimitInfo', { siteId: siteId, depositType: depositType, currencyId: currencyId, vipLevel: Level, depositMin: DepositMin, depositMax: DepositMax, depositStatus: depositStatus })
            .then(function (response) {
                if (response.data.errorCode === 0) {
                    props.ClickSave(newLevelList);
                    alert(response.data.message);
                } else {
                    alert(response.data.message);
                }
            }).catch(function (error) {
                alert(error);
            })
    }

    return (
        <div className="popup">
            <div className="popup_inner">
                <h1>Add new Vip level</h1>
                <form className="ui form">
                    <div className="field">
                        <label>Vip Level</label>
                        <input readOnly defaultValue={vipLevel} onChange={(e) => { setLevel(parseInt(e.target.value)) }} />
                    </div>
                    <div className="field">
                        <label>Deposit Min</label>
                        <input type="number" defaultValue={depositMin} onChange={(e) => { setDepositMin(parseInt(e.target.value)) }} />
                    </div>
                    <div className="field">
                        <label>Deposit Max</label>
                        <input type="number" defaultValue={depositMax} onChange={(e) => { setDepositMax(parseInt(e.target.value)) }} />
                    </div>
                    <button className="ui right floated primary button" onClick={handleSave} >Save</button>
                    <button className="ui right floated button" onClick={props.ClickCancel}>Cancel</button>
                    <div className="clearfix"></div>
                </form>
            </div>
        </div>
    )
}