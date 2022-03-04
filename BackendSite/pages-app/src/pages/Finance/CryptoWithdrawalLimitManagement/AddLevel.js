import React, { useState } from "react";
import WebAPI from '../../../module/WebAPI';
export const AddLevel = (props) => {
    let lastList = props.levelList.length === 0
        ? []
        : props.levelList[props.levelList.length - 1];

    let currentVipLevel = 1;
    let currentWithdrawalMin = 1;
    let currentWithdrawalMax = 1;

    if (lastList.length !== 0) {
        currentVipLevel = parseInt(lastList.vipLevel) + 1;
        currentWithdrawalMin = parseInt(lastList.withdrawalMin) + 1;
        currentWithdrawalMax = parseInt(lastList.withdrawalMax) + 1;
    }

    const [level, setLevel] = useState(currentVipLevel);
    const [withdrawalMin, setWithdrawalMin] = useState(currentWithdrawalMin);
    const [withdrawalMax, setWithdrawalMax] = useState(currentWithdrawalMax);

    const handleSave = (e) => {
        e.preventDefault();
        if (lastList.vipLevel >= level) { return (alert("VIP level error!")) };
        let newLevelList = {
            siteId: props.siteId,
            currencyId: props.currencyId,
            vipLevel: level,
            withdrawalMin: withdrawalMin,
            withdrawalMax: withdrawalMax,
            networkType: props.networkType,
            cryptoCurrency: props.cryptoCurrencyId
        }
        let param = {
            Url: "apiV2/WithdrawalLimitCrypto/Create",
            Data: newLevelList
          };
        WebAPI.PostData(param)
            .then(function (response) {
                if (response.data.errorCode === 0) {
                    props.clickSave(newLevelList);
                    props.doGetList();
                    alert(response.data.message || "Update successful.");
                } else {
                    alert(response.data.message || "Process failed.");
                }
            }).catch(function (error) {
                alert(error);
            })
    }

    return (
        <div style={{ marginTop: "10px", padding: "15px 20px", backgroundColor: "#fff" }}>
            <h3>Add new Vip level</h3>
            <form style={{ marginTop: "10px" }}>
                <div className="form-group row">
                    <label className="col-sm-2 col-form-label">Vip Level</label>
                    <input className="col-sm-2 form-control"
                        readOnly
                        defaultValue={currentVipLevel}
                        onChange={(e) => { setLevel(parseInt(e.target.value)) }} />
                </div>
                <div className="form-group row">
                    <label className="col-sm-2 col-form-label">Withdrawal Min</label>
                    <input className="col-sm-2 form-control"
                        type="number"
                        defaultValue={currentWithdrawalMin}
                        onChange={(e) => { setWithdrawalMin(parseInt(e.target.value)) }} />
                </div>
                <div className="form-group row">
                    <label className="col-sm-2 col-form-label">Withdrawal Max (0 = Unlimited)</label>
                    <input className="col-sm-2 form-control"
                        type="number"
                        defaultValue={currentWithdrawalMax}
                        onChange={(e) => { setWithdrawalMax(parseInt(e.target.value)) }} />
                </div>
                <div className="form-group" style={{ margin: "0px" }}>
                    <button className="btn btn-cancel" onClick={props.clickCancel}>Cancel</button>
                    <button className="btn btn-primary"
                        onClick={handleSave}
                        style={{ marginLeft: "10px" }}>Save</button>
                </div>
            </form>
        </div>

    )
}