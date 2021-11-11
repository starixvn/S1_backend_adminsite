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
        <div className="newwrapper">
            <div className="popup-wrap is-show">
                <div className="popup is-open">
                    <div className="popup__head"><span className="text">Clone Bonus</span>
                        <div className="newbtn"><i className="icon icon-close"></i></div>
                    </div>
                    <div className="popup__body os-host os-theme-dark os-host-resize-disabled os-host-scrollbar-horizontal-hidden os-host-scrollbar-vertical-hidden os-host-transition">
                        <div className="os-resize-observer-host">
                            <div className="os-resize-observer observed"></div>
                        </div>
                        <div className="os-size-auto-observer">
                            <div className="os-resize-observer observed"></div>
                        </div>
                        <div className="os-content-glue"></div>
                        <div className="os-padding">
                            <div className="os-viewport os-viewport-native-scrollbars-invisible">
                                <div className="os-content">
                                    <div className="popup__row">
                                        <div className="form">
                                            <div className="form__group">
                                                <div className="form__title">
                                                    <span className="text">Bonus Code</span>
                                                </div>
                                                <div className="textfield">
                                                    <InputBox
                                                        Title="BonusCode"
                                                        HideTitle={true}
                                                        OnChange={Action.SetCloneValue}
                                                        dataColumn="cloneBonusCode"
                                                        value={NewDatas.cloneBonusCode}
                                                        inputClass="textfield__input"
                                                    />
                                                </div>
                                                {Datas.isSpecial ? 
                                                <>
                                                    <div className="form__title">
                                                        <span className="text">Special Code</span>
                                                    </div>
                                                    <div className="textfield">
                                                        <InputBox
                                                            Title="SpecialCode"
                                                            HideTitle={true}
                                                            OnChange={Action.SetCloneValue}
                                                            dataColumn="cloneSpecialCode"
                                                            value={NewDatas.cloneSpecialCode}
                                                            inputClass="textfield__input"
                                                        />
                                                    </div>
                                                </>:null}                                             
                                            </div>
                                            <div className="table" style={{ marginTop: "1.25rem" }}>
                                                <table className="table table-striped">
                                                    <thead>
                                                        <tr>
                                                            <th>PromotionType</th>
                                                            <th>Description</th>
                                                            <th>WalletType</th>
                                                        </tr>
                                                    </thead>
                                                    <tbody>
                                                        <tr>
                                                            <td>{DefPromotionType[Datas.promotionType]}</td>
                                                            <td>{Datas.description}</td>
                                                            <td>{DefWalletType[Datas.walletType]?.displayText}</td>
                                                        </tr>
                                                    </tbody>
                                                </table>
                                            </div>
                                        </div>
                                    </div>
                                    <div className="popup__row">
                                        <div className="newbtn-bar">
                                            <div className="newbtn-bar__group">
                                                <div className="newbtn newbtn-primary newbtn-w-min" onClick={Action.SubmitClone}><span className="text">Save</span></div>
                                                <div className="newbtn newbtn-secondary newbtn-w-min" onClick={Action.ClickCancle}><span className="text">Cancel</span></div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div className="os-scrollbar os-scrollbar-horizontal os-scrollbar-unusable">
                            <div className="os-scrollbar-track os-scrollbar-track-off">
                                <div className="os-scrollbar-handle">
                                </div>
                            </div>
                        </div>
                        <div className="os-scrollbar os-scrollbar-vertical os-scrollbar-unusable">
                            <div className="os-scrollbar-track os-scrollbar-track-off">
                                <div className="os-scrollbar-handle">
                                </div>
                            </div>
                        </div>
                        <div className="os-scrollbar-corner"></div>
                    </div>
                </div>
            </div>
        </div>
    )
}