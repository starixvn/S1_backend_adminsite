import React, { useState } from "react";
import { api } from '../../../module/WebAPI';
export const PopBoxContentAddPoint = (props) => {
    
    let lastList = props.domainList.length === 0 ? [] : props.domainList[props.domainList.length - 1];
    let siteId = props.SiteId, currencyId = props.CurrencyId, domainName = null, sortNum = 1,  isActive = 1;
    if (lastList.length !== 0) {
        domainName = lastList.domainName;
        sortNum = props.domainList.length + 1;
    }
    const [Domain, setDomain] = useState(domainName);
    const [Sort, setSort] = useState(sortNum);
    const [IsActive, setStatus] = useState(isActive);
    let newDomainList = { siteId: siteId, currencyId: currencyId, domain: Domain, sort: Sort, status: IsActive }
    const handleSave = (e) => {
        e.preventDefault();

       // if (lastList.pointLevel >= Domain) { return (alert("err1")) };
        // if (lastList.depositCnt >= DepositCnt) { return (alert("Deposit Count too small")) };
        // if (lastList.withdrawalCnt >= WithdrawalCnt) { return (alert("Withdrawal Count too small")) };
        api.post('/api/DomainTable/SetDomain', newDomainList)
            .then(function (response) {
                if (response.data.errorCode === 0) {
                    props.ClickSave(newDomainList);
                    alert(response.data.message);
                } else {
                    alert(response.data.message); 
                }
            }).catch(function (error) {
                alert(error);
            })
    }

    return (
        <div className="newwrapper">
            <div className="popup-wrap is-show">
                <div className="popup is-open">
                    <div className="popup__head"><span className="text">Add new Domain</span>
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
                                                    <span className="text">Domain</span>
                                                </div>
                                                <div className="textfield">
                                                    <input type="text" defaultValue={domainName} onChange={(e) => { setDomain(e.target.value.toString()) }} />
                                                </div>
                                            </div>
                                            <div className="form__group">
                                                <div className="form__title">
                                                    <span className="text">Sort</span>
                                                </div>
                                                <div className="textfield">
                                                    <input type="number" defaultValue={sortNum} onChange={(e) => { setSort(parseInt(e.target.value)) }} />
                                                </div>
                                            </div>
                                            <div className="form__group">
                                                <div className="form__title">
                                                    <span className="text">Status</span>
                                                </div>
                                                <div className="textfield">
                                                    <input type="checkbox" checked={(isActive === 1)} onChange={(e) => { setStatus((e.target.checked) ? 1 : 0) }} />
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div className="popup__row">
                                        <div className="newbtn-bar">
                                            <div className="newbtn-bar__group">
                                                <div className="newbtn newbtn-primary newbtn-w-min" onClick={handleSave}><span className="text">Save</span></div>
                                                <div className="newbtn newbtn-secondary newbtn-w-min" onClick={props.ClickCancel}><span className="text">Cancel</span></div>
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