import React from "react"

if (process.env.NODE_ENV !== "production") {
    window.gCurrencyList = [2, 4, 13, 15, 51, 61];
    window.i18n = {
        lbl_Currency2: "MYR",
        lbl_Currency4: "THB",
        lbl_Currency13: "RMB",
        lbl_Currency15: "IDR",
        lbl_Currency51: "VND",
        lbl_Currency61: "INR",
    };
}

export default function CustInfoPanel ({ custData }) {
    return (
        <div>
            <h4 className="page-name">Cust Info</h4>
            <table className="table table-detail">
                <tbody>
                    <tr>
                        <th scope="row">SiteName</th>
                        <td>{custData.siteName}</td>
                    </tr>
                    <tr>
                        <th scope="row">User Name</th>
                        <td>{custData.userName}</td>
                    </tr>
                    <tr>
                        <th scope="row">Currency</th>
                        <td>{ window.i18n["lbl_Currency" + custData.currencyId]}</td>
                    </tr>
                </tbody>
            </table>
        </div>
    )
}