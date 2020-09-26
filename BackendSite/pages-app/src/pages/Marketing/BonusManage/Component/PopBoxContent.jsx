import React from "react";
import InputBox from "./InputBox";

export default ({
    Datas,
    NewDatas,
    DefPromotionType,
    DefWalletType,
    Action
}) => {

    return (
        <div className="popup">
            <div className="popup_inner">
                <h1>Clone Bonus</h1>
                <div className="field">
                    <InputBox
                        Title="BonusCode"
                        OnChange={Action.SetCloneValue}
                        dataColumn="cloneBonusCode"
                        value={NewDatas.cloneBonusCode}
                    />
                </div>
                <div className="field">
                    <label>PromotionType</label>
                    {DefPromotionType[Datas.promotionType]}
                </div>
                <div className="field">
                    <label>Description</label>
                    {Datas.description}
                </div>
                <div className="field">
                    <label>WalletType</label>
                    {DefWalletType[Datas.walletType]?.displayText}
                </div>
                <button className="ui right floated primary button" onClick={Action.SubmitClone} >Save</button>
                <button className="ui right floated button" onClick={Action.ClickCancle} >Cancel</button>
                <div className="clearfix"></div>
            </div>
        </div>
    )
}