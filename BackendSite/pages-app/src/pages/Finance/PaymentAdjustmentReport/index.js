import React, { useState, useRef, useEffect } from "react";
import ReactDOM from 'react-dom';
import Filter from './Filter';
import Result from './Result';
import WebAPI, { api } from '../../../module/WebAPI';
import qs from 'qs';
import moment from 'moment';

const App = () => {
    const [stateFilter, setFilter] = useState({
        'TransId': '',
        'Currency': 0,
        'StartDate': null,
        'EndDate': null,
        'User': '',
        'Type': 0,
        'CustId': 0
    });

    const [stateRef] = useState({
        f_trigger_Start: useRef(null),
        f_trigger_End: useRef(null),
        selectdate_end: useRef(null),
        selectdate_start: useRef(null),
    });

    const [stateDatas, setDatas] = useState({
        PageInfo: null,
        Datas: null,
        FilterInfo: null
    });

    const [stateEdit, setEdit] = useState({});
    const [editValue, setEditValue] = useState(null);
    const [adjustCategoryList, setAdjustCategoryList] = useState([]);
    const [stateDisabled, setDisabled] = useState(false);

    useEffect(() => {
        let f_trigger_Start = stateRef.f_trigger_Start.current;
        let f_trigger_End = stateRef.f_trigger_End.current;
        let selectdate_end = stateRef.selectdate_end.current;
        let selectdate_start = stateRef.selectdate_start.current;

        /*eslint-disable */
        Calendar.setup({
            inputField: selectdate_end, // id of the input field
            ifFormat: "mm/dd/y",        // format of the input field
            button: f_trigger_End,  // trigger for the calendar (button ID)
            singleClick: true              // double-click mode
        });

        Calendar.setup({
            inputField: selectdate_start, // id of the input field
            ifFormat: "mm/dd/y",        // format of the input field
            button: f_trigger_Start,  // trigger for the calendar (button ID)
            singleClick: true              // double-click mode
        });
        /*eslint-enable */
    }, [stateRef]);

    useEffect(() => {
        api.post('/apiV2/BaseData/GetReasonList', { FuncKey: 'Finance/PaymentAdjustment' })
            .then(function (response) {
                if (response.data.errorCode === 0) {
                    let list = [];
                    if (response.data.data !== null)
                    {
                        response.data.data.map(x => {
                            list.push({ name: x.description, value: x.reasonId });
                        })
                        setAdjustCategoryList(list);
                    }
                } else {
                    alert(!response.message ? 'Get reason list failed.' : response.message);
                }
            }).catch(function (error) {
                alert(error);
            })
    }, []);

    const Filter_OnChange = (v, dataColumn) => {
        setFilter({
            ...stateFilter,
            [dataColumn]: v
        });
    }

    const Filter_Submit = () => {
        let startDate = moment(stateRef.selectdate_start.current.value).unix();
        let endDate = moment(stateRef.selectdate_end.current.value).add(1, 'days').unix() - 1;

        let param = {
            Url: 'PaymentAdjustmentReport/GetPaymentAdjustmentReport',
            Data: qs.stringify({
                AdjustType: stateFilter.Type,
                CurrencyId: stateFilter.Currency,
                StartDate: startDate,
                EndDate: endDate,
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
                            rowCount: res.data.rowCount,
                            currencyAmount: res.data.currencyAmount
                        },
                        Datas: [...res.data.data],
                        FilterInfo: {
                            ...stateFilter,
                            StartDate: startDate,
                            EndDate: endDate
                        }
                    }
                );
                setEdit({});
            } else {
                alert(res.data.message ?? 'failed to get data.');
            }
        })
    }

    const Result_ChangePage = (PageNumber) => {
        let param = {
            Url: 'PaymentAdjustmentReport/GetPaymentAdjustmentReport',
            Data: qs.stringify({
                AdjustType: stateDatas.FilterInfo.Type,
                CurrencyId: stateDatas.FilterInfo.Currency,
                StartDate: stateDatas.FilterInfo.StartDate,
                EndDate: stateDatas.FilterInfo.EndDate,
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
                setEdit({});
            } else {
                alert(res.data.message ?? 'failed to get data.');
            }
        })
    }

    const Result_ExportToExcel = () => {
        let param = {
            Url: 'PaymentAdjustmentReport/ExportPaymentAdjustmentReport',
            Data: {
                AdjustType: stateDatas.FilterInfo.Type,
                CurrencyId: stateDatas.FilterInfo.Currency,
                StartDate: stateDatas.FilterInfo.StartDate,
                EndDate: stateDatas.FilterInfo.EndDate,
                UserName: stateDatas.FilterInfo.User,
                TransId: stateDatas.FilterInfo.TransId
            }
        };

        WebAPI.DownloadData(param);
    }
    const Result_HandleEdit = (type, idx, value) => {
        setEditValue({
            type: type,
            idx: idx,
            value: value
        });
        setEdit({ type: type, idx: idx });
    };
    const Result_HandleCancel = () => {
        setEdit({});
    };
    const Result_EditHandleUpdate = () => {
        setDisabled(true);
        let data = stateDatas.Datas[editValue.idx];
        let transId = data.transId;
        let beforeCategory = data.adjustCategory;
        let beforeMemo = data.memo;
        let afterCategory = editValue.type === 'reason' ? editValue.value : data.adjustCategory;
        let afterMemo = editValue.type === 'memo' ? editValue.value : data.memo;

        //console.log(transId, category, memo);
        let param = {
            Url: 'PaymentAdjustmentReport/SetAdjustment',
            Data: qs.stringify({
                TransId: transId,
                BeforeCategory: beforeCategory,
                AfterCategory: afterCategory,
                BeforeMemo: beforeMemo,
                AfterMemo: afterMemo
            })
        };
        
        WebAPI.PostData(param).then((res) => {
            if (res.data.errorCode === 0) {
                Filter_Submit();
                alert(!res.data.message ? 'Update successful.' : res.data.message);
            } else {
                alert(!res.data.message ? 'Failed to update data.' : res.data.message);
            }
            setDisabled(false);
        });
    };

    const Result_EditChangeData = (value) => {
        //console.log({ ...editValue, value: value });
        setEditValue({ ...editValue, value: value });
    };

    return (
        <div className="container-fluid">
            <h4 className="page-name">Payment Adjustment</h4>
            <Filter filterInfo={stateFilter} refInfo={stateRef} onChange={Filter_OnChange} ClickSubmit={Filter_Submit} />
            {
                stateDatas.Datas === null
                    ? null
                    : <Result
                        {...stateDatas}
                        ChangePage={Result_ChangePage}
                        ExportToExcel={Result_ExportToExcel}
                        HandleEdit={Result_HandleEdit}
                        HandleCancel={Result_HandleCancel}
                        HandleUpdate={Result_EditHandleUpdate}
                        ChangeData={Result_EditChangeData}
                        StateEdit={stateEdit}
                        EditValue={editValue}
                        AdjustCategoryList={adjustCategoryList}
                        Disabled={stateDisabled}
                    />
            }
        </div>
    );
}

export default App;

if (process.env.NODE_ENV === 'production') {
    ReactDOM.render(<App />, document.getElementById('content'))
}