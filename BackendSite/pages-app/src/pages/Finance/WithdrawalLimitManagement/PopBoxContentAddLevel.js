import React, { useState } from "react";
import { api } from '../../../module/WebAPI';
export const PopBoxContentAddLevel = (props) => {
    let lastList = props.LevelList.length === 0 ? [] : props.LevelList[props.LevelList.length - 1];
    let siteId = props.SiteId, currencyId = props.CurrencyId, vipLevel = 1, withdrawalMin = 1, withdrawalMax = 1;

    if (lastList.length !== 0) {
        vipLevel = parseInt(lastList.vipLevel) + 1;
        withdrawalMin = parseInt(lastList.withdrawalMin) + 1;
        withdrawalMax = parseInt(lastList.withdrawalMax) + 1;
    }

    const [Level, setLevel] = useState(vipLevel);
    const [WithdrawalMin, setWithdrawalMin] = useState(withdrawalMin);
    const [WithdrawalMax, setWithdrawalMax] = useState(withdrawalMax);
    let newLevelList = { siteId: siteId, currencyId: currencyId, vipLevel: Level, withdrawalMin: WithdrawalMin, withdrawalMax: WithdrawalMax }
    const handleSave = (e) => {
        e.preventDefault();

        if (lastList.vipLevel >= Level) { return (alert("err1")) };
        //if (lastList.withdrawalMin >= WithdrawalMin) { return (alert("Withdrawal Min too small")) };
        if (lastList.withdrawalMax >= WithdrawalMax) { return (alert("Withdrawal Max too small")) };
        api.post('/api/Withdrawal/AddWithdrawalLimitInfo', { siteId: siteId, currencyId: currencyId, vipLevel: Level, withdrawalMin: WithdrawalMin, withdrawalMax: WithdrawalMax })
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
        <div>
            <div>
                <h1>Add new Vip level</h1>
                <form className="ui form">
                    <div className="field">
                        <label>Vip Level</label>
                        <input readOnly defaultValue={vipLevel} onChange={(e) => { setLevel(parseInt(e.target.value)) }} />
                    </div>
                    <div className="field">
                        <label>Withdrawal Min</label>
                        <input type="number" defaultValue={withdrawalMin} onChange={(e) => { setWithdrawalMin(parseInt(e.target.value)) }} />
                    </div>
                    <div className="field">
                        <label>Withdrawal Max</label>
                        <input type="number" defaultValue={withdrawalMax} onChange={(e) => { setWithdrawalMax(parseInt(e.target.value)) }} />
                        <label>(0 = Unlimited)</label>
                    </div>
                    <button className="ui right floated primary button" onClick={handleSave} >Save</button>
                    <button className="ui right floated button" onClick={props.ClickCancel}>Cancel</button>
                    <div className="clearfix"></div>
                </form>
            </div>
        </div>
    )
}