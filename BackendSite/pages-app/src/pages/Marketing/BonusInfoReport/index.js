import React, { useState, useRef, useEffect } from "react";
import ReactDOM from 'react-dom';
import Filter from './Filter';
import Result from './Result';
import WebAPI from '../../../module/WebAPI';
import qs from 'qs';
import moment from 'moment';

const App = () => {
    const [stateFilter, setFilter] = useState({
        'SelectSite': [],
        'TransId': '',
        'Currency': 0,
        'StartDate': moment().add(-10, 'days').format('l'),
        'EndDate': moment().format('l'),
        'User': '',
        'Type': 0,
        'BonusCode': ''
    });

    const [stateDatas, setDatas] = useState({
        PageInfo: null,
        Datas: null,
        FilterInfo: null
    });

    const [siteList, setSiteList] = useState();

    useEffect(() => {
        let param = {
            Url: 'Common/AdminUserSiteList'
        };
        WebAPI.PostData(param).then((res) => {
            if (res.data.errorCode === 0) {
                setSiteList(res.data.data);
            } else {
                alert(res.data.message ?? 'failed to get data.');
            }
        })
    }, []);

    const Filter_OnChange = (v, dataColumn) => {
        setFilter({
            ...stateFilter,
            [dataColumn]: v
        });
    }

    const Filter_CheckBox_OnChange = (v, dataColumn) => {
        let isRemove = stateFilter[dataColumn].includes(v);
        let newChecked = isRemove
            ? stateFilter[dataColumn].filter(value => value !== v)
            : stateFilter[dataColumn].push(v);
        let currentTab = stateFilter[dataColumn];
        if (newChecked.length === 0) {
            currentTab = [];
        } else if (currentTab === null) {
            currentTab = newChecked;
        }

        if (isRemove && !newChecked.includes(currentTab)) {
            currentTab = newChecked.length === 0 ? [] : newChecked;
        }
        setFilter({
            ...stateFilter,
            [dataColumn]: currentTab
        });
    }

    const Filter_CheckBox_CheckAll = (v, dataColumn, options) => {
        if (v !== "All") return false;
        let currentTab = (stateFilter[dataColumn].length === options.length)
            ? []
            : options.map(o => {
                return o.value
            })
        setFilter({
            ...stateFilter,
            [dataColumn]: currentTab
        });
    }

    const Filter_Submit = () => {
        if (stateFilter.SelectSite.length === 0) {
            alert("Please Choose at least one Site")
            return false;
        }
        let param = {
            Url: 'BonusInfo/GetBonusInfoReport',
            Data: qs.stringify({
                SelectSite: stateFilter.SelectSite,
                BonusCode: stateFilter.BonusCode,
                PromotionType: stateFilter.Type,
                CurrencyId: stateFilter.Currency,
                StartDate: moment(stateFilter.StartDate).unix(),
                EndDate: moment(stateFilter.EndDate).add(1, 'days').unix() - 1,
                UserName: stateFilter.User,
                TransId: stateFilter.TransId,
                PageNumber: 1
            })
        };

        WebAPI.PostData(param).then((res) => {
            if (res.data.errorCode === 0) {
                setDatas(
                    {
                        PageInfo: {
                            totalPages: res.data.totalPages,
                            pageNumber: 1,
                            rowCount: res.data.rowCount
                        },
                        Datas: [...res.data.data],
                        FilterInfo: {
                            ...stateFilter
                        }
                    }
                )
            } else {
                alert(res.data.message ?? 'failed to get data.');
            }
        })
    }

    const Result_ChangePage = (PageNumber) => {
        if (stateDatas.FilterInfo.SelectSite.length === 0) {
            alert("Please Choose at least one Site")
            return false;
        }
        let param = {
            Url: 'BonusInfo/GetBonusInfoReport',
            Data: qs.stringify({
                SelectSite: stateDatas.FilterInfo.SelectSite,
                BonusCode: stateDatas.FilterInfo.BonusCode,
                PromotionType: stateDatas.FilterInfo.Type,
                CurrencyId: stateDatas.FilterInfo.Currency,
                StartDate: moment(stateDatas.FilterInfo.StartDate).unix(),
                EndDate: moment(stateDatas.FilterInfo.EndDate).add(1, 'days').unix() - 1,
                UserName: stateDatas.FilterInfo.User,
                TransId: stateDatas.FilterInfo.TransId,
                PageNumber: PageNumber
            })
        };

        WebAPI.PostData(param).then((res) => {
            if (res.data.errorCode === 0) {
                setDatas({
                    ...stateDatas,
                    PageInfo: {
                        ...stateDatas.PageInfo,
                        pageNumber: PageNumber
                    },
                    Datas: [...res.data.data],
                });
            } else {
                alert(res.data.message ?? 'failed to get data.');
            }
        })
    }

    const Result_ExportToExcel = () => {
        if (stateDatas.FilterInfo.SelectSite.length === 0) {
            alert("Please Choose at least one Site")
            return false;
        }
        let param = {
            Url: 'BonusInfo/ExportBonusInfoReport',
            Data: {
                SelectSite: JSON.stringify(stateDatas.FilterInfo.SelectSite),
                BonusCode: stateDatas.FilterInfo.BonusCode,
                PromotionType: stateDatas.FilterInfo.Type,
                CurrencyId: stateDatas.FilterInfo.Currency,
                StartDate: moment(stateDatas.FilterInfo.StartDate).unix(),
                EndDate: moment(stateDatas.FilterInfo.EndDate).add(1, 'days').unix() - 1,
                UserName: stateDatas.FilterInfo.User,
                TransId: stateDatas.FilterInfo.TransId
            }
        };

        WebAPI.DownloadData(param);
    }
    return (
        <div className="container-fluid">
            <h4 className="page-name">Bonus Report</h4>
            <Filter filterInfo={stateFilter} onChange={Filter_OnChange} onChangeCheckBox={Filter_CheckBox_OnChange} onChangeAllCheckBox={Filter_CheckBox_CheckAll} ClickSubmit={Filter_Submit} SiteList={siteList} />
            {stateDatas.Datas === null ? null : <Result {...stateDatas} ChangePage={Result_ChangePage} ExportToExcel={Result_ExportToExcel} />}
        </div>
    );
}

export default App;

if (process.env.NODE_ENV === 'production') {
    ReactDOM.render(<App />, document.getElementById('content'))
}