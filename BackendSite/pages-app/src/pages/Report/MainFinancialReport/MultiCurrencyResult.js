import React, { useState } from "react";
import { AgGridReact } from 'ag-grid-react';
import { AllCommunityModules } from "@ag-grid-community/all-modules";
import "@ag-grid-community/all-modules/dist/styles/ag-grid.css";
import "@ag-grid-community/all-modules/dist/styles/ag-theme-blue.css";

const defaultColDef = {
    sortable: true
}

const dataColumnDefs = (props) => {
    return [
        {
            headerName: 'Date',
            field: "date",
        },
        // {
        //     columnTitle: 'Currency',
        //     dataColumn: "isoCode"
        // },
        {
            headerName: "Deposit",
            children: [
                // {
                //     headerName: "Total in MYR",
                //     field: "depositSummary",
                //     width: 200,
                //     columnGroupShow: "closed",
                // },
                ...props.CurrencyList.map(curr => {
                    return (
                        {
                            headerName: curr.code,
                            field: `deposit_${curr.id}`,
                            // columnGroupShow: "open",
                        }
                    )
                })
            ]
        },
        {
            headerName: 'Withdrawal',
            children: [
                // {
                //     headerName: "Total in MYR",
                //     field: "withdrawalSummary",
                //     width: 200,
                //     columnGroupShow: "closed",
                // },
                ...props.CurrencyList.map(curr => {
                    return (
                        {
                            headerName: curr.code,
                            field: `withdrawal_${curr.id}`,
                            // columnGroupShow: "open",
                        }
                    )
                })
            ]
        },
        {
            headerName: "Bonus",
            children: [
                // {
                //     headerName: "Total in MYR",
                //     field: "bonusSummary",
                //     width: 200,
                //     columnGroupShow: "closed",
                // },
                ...props.CurrencyList.map(curr => {
                    return (
                        {
                            headerName: curr.code,
                            field: `bonus_${curr.id}`,
                            // columnGroupShow: "open",
                        }
                    )
                })
            ]
        },
        {
            headerName: "Rebate",
            children: [
                // {
                //     headerName: "Total in MYR",
                //     field: "rebateSummary",
                //     width: 200,
                //     columnGroupShow: "closed",
                // },
                ...props.CurrencyList.map(curr => {
                    return (
                        {
                            headerName: curr.code,
                            field: `rebate_${curr.id}`,
                            // columnGroupShow: "open",
                        }
                    )
                })
            ]
        },
        {
            headerName: "Adjustment",
            children: [
                // {
                //     headerName: "Total in MYR",
                //     field: "adjustmentSummary",
                //     width: 200,
                //     columnGroupShow: "closed",
                // },
                ...props.CurrencyList.map(curr => {
                    return (
                        {
                            headerName: curr.code,
                            field: `adjustment_${curr.id}`,
                            // columnGroupShow: "open",
                        }
                    )
                })
            ]
        },
        {
            headerName: "TurnOver(Stake)",
            children: [
                // {
                //     headerName: "Total in MYR",
                //     field: "turnOverStakeSummary",
                //     width: 200,
                //     columnGroupShow: "closed",
                // },
                ...props.CurrencyList.map(curr => {
                    return (
                        {
                            headerName: curr.code,
                            field: `turnOverStake_${curr.id}`,
                            // columnGroupShow: "open",
                        }
                    )
                })
            ]
        },
        {
            headerName: "TurnOver(ValidBet)",
            children: [
                // {
                //     headerName: "Total in MYR",
                //     field: "turnOverValidBetSummary",
                //     width: 200,
                //     columnGroupShow: "closed",
                // },
                ...props.CurrencyList.map(curr => {
                    return (
                        {
                            headerName: curr.code,
                            field: `turnOverValidBet_${curr.id}`,
                            // columnGroupShow: "open",
                        }
                    )
                })
            ]
        },
        {
            headerName: "WinLoss",
            children: [
                // {
                //     headerName: "Total in MYR",
                //     field: "winLossSummary",
                //     width: 200,
                //     columnGroupShow: "closed",
                // },
                ...props.CurrencyList.map(curr => {
                    return (
                        {
                            headerName: curr.code,
                            field: `winLoss_${curr.id}`,
                            // columnGroupShow: "open",
                        }
                    )
                })
            ]
        }
    ]
}

const Panel = (props) => {

    // console.log("multi-props",props.Datas);

    // const columnDefs = dataColumnDefs(props);
    // let headerRows = [];

    // columnDefs.forEach((element, idx) => {
    //     headerRows.push(
    //         <th key={`th-${element.dataColumn}-${idx}`} scope="col" className={element.header && element.header.attr && element.header.attr.class ? element.header.attr.class : ''}>
    //             {element.columnTitle}
    //         </th>
    //     );
    // });

    // let bodyRows = props.Datas.map((data, idx) => {
    //     let row = columnDefs.map((element, idx) => {
    //         let subData = {};
    //         if (element.body && element.body.subData) {
    //             element.body.subData.forEach(s => { subData[s] = data[s] });
    //         }
    //         return (
    //             <td key={`td-${element.dataColumn}-${idx}`} className={element.body && element.body.attr && element.body.attr.class ? element.body.attr.class : ''}>
    //                 {element.body && element.body.format ? element.body.format(data[element.dataColumn], subData) : data[element.dataColumn]}
    //             </td>
    //         );
    //     });



    //     return (
    //         <tr key={`result-body-${idx}`}>
    //             {row}
    //         </tr>
    //     )
    // });

    const [stateData, setData] = useState({
        Datas: props.Datas,
        // TotalDatas:props.TotalDatas
    });

    // const [gridApi, setGridApi] = useState(null);
    // const [gridColumnApi, setGridColumnApi] = useState(null);
    const onGridReady = (params) => {
        setData({
            Datas:props.Datas,
            // TotalDatas:props.TotalDatas
        });
        // setGridApi(params.api);
        // setGridColumnApi(params.columnApi);

        // params.api.sizeColumnsToFit();
        // console.log(params);
    }
    return (
        <div id="myGrid" className="ag-theme-blue table" style={{ height: '500px' }}>
            <div className="table-options">
                <button type="button" className="btn btn-approved" onClick={props.ExportToExcel}>Export Excel</button>
            </div>
            <AgGridReact
                modules={AllCommunityModules}
                columnDefs={dataColumnDefs(props)}
                defaultColDef={defaultColDef}
                debug={false}
                rowData={stateData.Datas}
                onGridReady={onGridReady}
                minColWidth={150}
                // rowHeight={20}
            />
            {/* <div style={{ height: '50%' }} className="ag-theme-balham">
                <AgGridReact
                    modules={AllCommunityModules}
                    columnDefs={columnTotalDefs(props)}
                    defaultColDef={defaultColDef}
                    debug={false}
                    rowData={stateData.TotalDatas}
                    onGridReady={onGridReady}
                />
            </div> */}
        </div>
    )
}

export default Panel;